-- Slasher Showdown
local s,id=GetID()
function s.initial_effect(c)
    -- Activate: Send 1 Level 8 Slasher to GY, then optionally remove 1 Kill Counter to search a Quick-Play
    local e1=Effect.CreateEffect(c)
    e1:SetCategory(CATEGORY_TOGRAVE+CATEGORY_TOHAND+CATEGORY_SEARCH)
    e1:SetType(EFFECT_TYPE_ACTIVATE)
    e1:SetCode(EVENT_FREE_CHAIN)
    e1:SetTarget(s.target)
    e1:SetOperation(s.activate)
    c:RegisterEffect(e1)

    -- GY Trigger: Banish to apply 1 of 2 effects
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,1))
    e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_O)
    e2:SetCode(EVENT_LEAVE_FIELD)
    e2:SetProperty(EFFECT_FLAG_DELAY)
    e2:SetRange(LOCATION_GRAVE)
    e2:SetCondition(s.gycon)
    e2:SetCost(aux.bfgcost) -- Banishes this card from GY as cost
    e2:SetTarget(s.gytg)
    e2:SetOperation(s.gyop)
    c:RegisterEffect(e2)
end
s.counter_list={0x199}

-- Filter for sending to GY
function s.tgfilter(c)
    return c:IsSetCard(0x991) and c:IsLevel(8) and c:IsAbleToGrave()
end

-- Filter for searching Quick-Play Spell
function s.thfilter(c)
    return c:IsSetCard(0x991) and c:IsType(TYPE_SPELL) and c:IsType(TYPE_QUICKPLAY) and c:IsAbleToHand()
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.IsExistingMatchingCard(s.tgfilter,tp,LOCATION_DECK,0,1,nil) end
    Duel.SetOperationInfo(0,CATEGORY_TOGRAVE,nil,1,tp,LOCATION_DECK)
    -- The search is optional and dependent on the counter removal, so it's not strictly required in OpInfo
end

function s.activate(e,tp,eg,ep,ev,re,r,rp)
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TOGRAVE)
    local g=Duel.SelectMatchingCard(tp,s.tgfilter,tp,LOCATION_DECK,0,1,1,nil)
    if #g>0 and Duel.SendtoGrave(g,REASON_EFFECT)>0 and g:GetFirst():IsLocation(LOCATION_GRAVE) then
        -- Check if we can apply the "then" effect
        local b1=Duel.IsCanRemoveCounter(tp,1,1,0x199,1,REASON_EFFECT)
        local b2=Duel.IsExistingMatchingCard(s.thfilter,tp,LOCATION_DECK,0,1,nil)

        if b1 and b2 and Duel.SelectYesNo(tp,aux.Stringid(id,0)) then -- "Remove 1 Kill Counter to search?"
            if Duel.RemoveCounter(tp,1,1,0x199,1,REASON_EFFECT) then
                Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_ATOHAND)
                local sg=Duel.SelectMatchingCard(tp,s.thfilter,tp,LOCATION_DECK,0,1,1,nil)
                if #sg>0 then
                    Duel.SendtoHand(sg,nil,REASON_EFFECT)
                    Duel.ConfirmCards(1-tp,sg)
                end
            end
        end
    end
end

-- Check if a Level 8 "Slasher" monster(s) left the field
function s.cfilter(c)
    return c:IsPreviousLocation(LOCATION_MZONE) and c:IsPreviousPosition(POS_FACEUP)
        and c:GetPreviousLevelOnField()==8 and c:IsPreviousSetCard(0x991)
end

function s.gycon(e,tp,eg,ep,ev,re,r,rp)
    return eg:IsExists(s.cfilter,1,nil) and not eg:IsContains(e:GetHandler())
end

function s.gytg(e,tp,eg,ep,ev,re,r,rp,chk)
    local b1=Duel.IsExistingMatchingCard(Card.IsCanAddCounter,tp,LOCATION_ONFIELD,0,1,nil,0x199,2)
    local b2=Duel.IsExistingMatchingCard(Card.IsAbleToDeck,tp,LOCATION_REMOVED,0,1,e:GetHandler())

    if chk==0 then return b1 or b2 end

    local op=0
    if b1 and b2 then
        op=Duel.SelectOption(tp,aux.Stringid(id,2),aux.Stringid(id,3))
    elseif b1 then
        op=Duel.SelectOption(tp,aux.Stringid(id,2))
    else
        op=Duel.SelectOption(tp,aux.Stringid(id,3))+1
    end
    e:SetLabel(op)

    if op==0 then
        Duel.SetOperationInfo(0,CATEGORY_COUNTER,nil,1,tp,LOCATION_ONFIELD)
    else
        Duel.SetOperationInfo(0,CATEGORY_TODECK,nil,1,tp,LOCATION_REMOVED)
    end
end

function s.gyop(e,tp,eg,ep,ev,re,r,rp)
    local op=e:GetLabel()
    if op==0 then
        -- Place 2 Kill Counters
        Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_FACEUP)
        local g=Duel.SelectMatchingCard(tp,Card.IsCanAddCounter,tp,LOCATION_ONFIELD,0,1,1,nil,0x199,2)
        if #g>0 then
            Duel.HintSelection(g)
            g:GetFirst():AddCounter(0x199,2)
        end
    else
        -- Shuffle 1 banished card into Deck
        Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_TODECK)
        local g=Duel.SelectMatchingCard(tp,Card.IsAbleToDeck,tp,LOCATION_REMOVED,0,1,1,e:GetHandler())
        if #g>0 then
            Duel.HintSelection(g)
            Duel.SendtoDeck(g,nil,SEQ_DECKSHUFFLE,REASON_EFFECT)
        end
    end
end