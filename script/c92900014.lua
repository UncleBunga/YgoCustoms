-- Slasher Barbed Wire
local s,id=GetID()
function s.initial_effect(c)
    -- Activate on card added to hand/field
    local e1=Effect.CreateEffect(c)
    e1:SetDescription(aux.Stringid(id,0))
    e1:SetCategory(CATEGORY_TOGRAVE+CATEGORY_COUNTER)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_TO_HAND)
    e1:SetProperty(EFFECT_FLAG_DELAY)
    e1:SetCondition(s.condition)
    e1:SetTarget(s.target)
    e1:SetOperation(s.operation)
    c:RegisterEffect(e1)
    
    -- Clones to catch cards being Special Summoned or Set directly from the Deck
    local e2=e1:Clone()
    e2:SetCode(EVENT_SPSUMMON_SUCCESS)
    c:RegisterEffect(e2)
    local e3=e1:Clone()
    e3:SetCode(EVENT_SSET)
    c:RegisterEffect(e3)
    local e4=e1:Clone()
    e4:SetCode(EVENT_MSET)
    c:RegisterEffect(e4)
end
s.counter_list={0x199}

-- Filter: Card was in Deck, is now controlled by opponent, and is in Hand or Field
function s.cfilter(c,tp)
    return c:IsPreviousLocation(LOCATION_DECK) and c:IsControler(1-tp)
        and (c:IsLocation(LOCATION_HAND) or c:IsLocation(LOCATION_ONFIELD))
end

-- Filter: Face-up card on your field that can receive a Kill Counter
function s.ctfilter(c)
    return c:IsFaceup() and c:IsCanAddCounter(0x199,1)
end

function s.condition(e,tp,eg,ep,ev,re,r,rp)
    -- Cannot activate during the Draw Phase
    if Duel.GetCurrentPhase()==PHASE_DRAW then return false end
    return eg:IsExists(s.cfilter,1,nil,tp)
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return true end
    local g=eg:Filter(s.cfilter,nil,tp)
    Duel.SetTargetCard(g) 
    Duel.SetOperationInfo(0,CATEGORY_TOGRAVE,g,#g,0,0)
end

function s.operation(e,tp,eg,ep,ev,re,r,rp)
    local g=Duel.GetTargetCards(e):Filter(s.cfilter,nil,tp)
    -- If the cards are still there, send them to the GY
    if #g>0 and Duel.SendtoGrave(g,REASON_EFFECT)>0 then
        local og=Duel.GetOperatedGroup()
        -- Confirm at least one actually hit the GY before offering the counter
        if og:IsExists(Card.IsLocation,1,nil,LOCATION_GRAVE) then
            if Duel.IsExistingMatchingCard(s.ctfilter,tp,LOCATION_ONFIELD,0,1,nil)
                and Duel.SelectYesNo(tp,aux.Stringid(id,1)) then
                Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_COUNTER)
                local cg=Duel.SelectMatchingCard(tp,s.ctfilter,tp,LOCATION_ONFIELD,0,1,1,nil)
                if #cg>0 then
                    cg:GetFirst():AddCounter(0x199,1)
                end
            end
        end
    end
end