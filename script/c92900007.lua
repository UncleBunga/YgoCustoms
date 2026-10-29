-- Slasher - The Caller
local s,id=GetID()
function s.initial_effect(c)
    c:EnableCounterPermit(0x199)
    -- Main effect: Opponent chooses random hand card and declares type
    local e1=Effect.CreateEffect(c)
    e1:SetDescription(aux.Stringid(id,0))
    e1:SetCategory(CATEGORY_DESTROY+CATEGORY_TOGRAVE)
    e1:SetType(EFFECT_TYPE_IGNITION)
    e1:SetRange(LOCATION_MZONE)
    e1:SetCountLimit(1)
    e1:SetTarget(s.target)
    e1:SetOperation(s.operation)
    c:RegisterEffect(e1)

    -- End Phase float if sent to GY this turn: Special Summon 1 "Slasher - The Caller" from hand or Deck
    local e2=Effect.CreateEffect(c)
    e2:SetDescription(aux.Stringid(id,1))
    e2:SetCategory(CATEGORY_SPECIAL_SUMMON)
    e2:SetType(EFFECT_TYPE_FIELD+EFFECT_TYPE_TRIGGER_O)
    e2:SetCode(EVENT_PHASE+PHASE_END)
    e2:SetRange(LOCATION_GRAVE)
    e2:SetCountLimit(1,{id,1})
    e2:SetCondition(s.spcon)
    e2:SetTarget(s.sptg)
    e2:SetOperation(s.spop)
    c:RegisterEffect(e2)
end

function s.target(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.GetFieldGroupCount(tp,LOCATION_HAND,0)>0 end
end

function s.operation(e,tp,eg,ep,ev,re,r,rp)
    if Duel.GetFieldGroupCount(tp,LOCATION_HAND,0)==0 then return end

    -- Opponent randomly selects 1 card from your hand
    local g=Duel.GetFieldGroup(tp,LOCATION_HAND,0):RandomSelect(1-tp,1)
    local tc=g:GetFirst()
    if not tc then return end

    -- Opponent declares a card type (Monster, Spell, or Trap)
    Duel.Hint(HINT_SELECTMSG,1-tp,HINTMSG_CARDTYPE)
    local op=Duel.SelectOption(1-tp,DECLTYPE_MONSTER,DECLTYPE_SPELL,DECLTYPE_TRAP)

    -- Reveal the card to the opponent and shuffle the hand back
    Duel.ConfirmCards(1-tp,tc)
    Duel.ShuffleHand(tp)

    -- Check if declaration was correct or incorrect
    local correct = (op==0 and tc:IsMonster()) or (op==1 and tc:IsSpell()) or (op==2 and tc:IsTrap())

    if not correct then
        -- Incorrect: Destroy 1 card opponent controls (if any exist)
        local dg=Duel.GetMatchingGroup(aux.TRUE,tp,0,LOCATION_ONFIELD,nil)
        if #dg>0 then
            Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_DESTROY)
            local des=dg:Select(tp,1,1,nil)
            Duel.Destroy(des,REASON_EFFECT)
        end
    else
        -- Correct: Send that card to the GY
        Duel.SendtoGrave(tc,REASON_EFFECT)
    end
end

-- End Phase Special Summon Condition: Sent to GY this turn
function s.spcon(e,tp,eg,ep,ev,re,r,rp)
    local c=e:GetHandler()
    return c:IsLocation(LOCATION_GRAVE) and c:GetTurnID()==Duel.GetTurnCount()
end

-- Special Summon Filter
function s.spfilter(c,e,tp)
    return c:IsCode(id) and c:IsCanBeSpecialSummoned(e,0,tp,false,false)
end

function s.sptg(e,tp,eg,ep,ev,re,r,rp,chk)
    if chk==0 then return Duel.GetLocationCount(tp,LOCATION_MZONE)>0
        and Duel.IsExistingMatchingCard(s.spfilter,tp,LOCATION_HAND+LOCATION_DECK,0,1,nil,e,tp) end
    Duel.SetOperationInfo(0,CATEGORY_SPECIAL_SUMMON,nil,1,tp,LOCATION_HAND+LOCATION_DECK)
end

function s.spop(e,tp,eg,ep,ev,re,r,rp)
    if Duel.GetLocationCount(tp,LOCATION_MZONE)<=0 then return end
    Duel.Hint(HINT_SELECTMSG,tp,HINTMSG_SPSUMMON)
    local g=Duel.SelectMatchingCard(tp,s.spfilter,tp,LOCATION_HAND+LOCATION_DECK,0,1,1,nil,e,tp)
    if #g>0 then
        Duel.SpecialSummon(g,0,tp,tp,false,false,POS_FACEUP)
    end
end